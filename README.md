# DevMarket — Software/API-Key/SaaS Marketplace

A .NET 10 microservices project for a **digital marketplace**: software licenses, API-key access,
SaaS subscriptions, and project bundles, sold with real JWT/RBAC identity (Admin vs Customer).
Independent services per domain, REST between frontend and backend, RabbitMQ/MassTransit for the
order→payment flow, Redis for cart and notification state, Razorpay for payments, SignalR for
live updates, YARP as the single public gateway, and a React frontend.

This started as a physical-goods e-commerce learning project (see git history/older commits for
that shape) and was pivoted into a digital-goods marketplace: **Inventory Service was removed
entirely** — a digital catalog has no stock to reserve — and **Identity Service was added** to
replace the old `X-Customer-Id` header placeholder with real login. See "The Digital Marketplace
Pivot" below for what changed and why.

## Architecture at a glance

```text
React (Vite)  ──▶  YARP Gateway (:5000)  ──▶  Identity / Product / Customer / Cart / Order / Payment / Notification
                                                    │
                                RabbitMQ (MassTransit) + Redis + SQL Server + Razorpay
```

- **Identity, Product, Customer, Order, Payment** — Vertical Slice Architecture: Minimal API
  endpoints (one `IEndpoint` per route) dispatch to a Command/Query handler living in its own
  `Features/<Name>/` folder, instead of a Controller calling into a shared per-entity service
  class. `Domain` and the EF Core `DbContext`/repository stay per-service shared infrastructure —
  see "Vertical Slice Architecture" below for the full shape.
- **Cart, Notification** — Redis-only, no SQL, Controllers (never Clean Architecture to begin
  with) — but both are JWT-authenticated like everything else (see "RBAC / Identity" below).
- **Gateway** — YARP reverse proxy; the only backend port exposed to the browser. Auth is
  enforced per-service (via `AddJwtAuthentication`), not at the Gateway.
- **BuildingBlocks/Auth** — `Roles`, `Permissions`, `AppClaimTypes`, `JwtOptions`,
  `ClaimsPrincipalExtensions`, and `AddJwtAuthentication(...)` (JWT validation + one
  authorization policy per permission) — shared by every service, issued only by Identity Service.
- **BuildingBlocks/Contracts** — shared integration events (`OrderConfirmedEvent`,
  `UserRegisteredEvent`, etc.) and a couple of genuinely shared primitives (`PagedResult<T>`,
  domain exceptions incl. `UnauthorizedAppException`/`ForbiddenAppException`).
- **BuildingBlocks/Application** — a from-scratch, dependency-free mediator (`IRequest`,
  `IRequestHandler`, `IPipelineBehavior`, `IMediator`) plus two pipeline behaviors
  (`ValidationBehaviour`, `CachingBehaviour`), shared by every sliced service's Application layer.
- **BuildingBlocks/WebApi** — `IEndpoint`/`AddEndpoints`/`MapEndpoints` (Minimal API endpoint
  auto-discovery), the one shared `AppExceptionHandler`, and the correlation-id middleware —
  shared by every sliced service's Api layer.

## The Digital Marketplace Pivot

- **Catalog**: `Product` gained `ProductType` (`License` / `ApiAccess` / `SaaSSubscription` /
  `Project`), `PricingModel` (`OneTime` / `Monthly` / `Yearly` — a label only, no recurring
  billing engine behind it), and `AssetUrl` (the repo/download/docs link handed to the buyer).
- **No stock, no saga hop**: with nothing to reserve, `CreateOrder` confirms the order and
  publishes `OrderConfirmedEvent` immediately — `OrderCreatedEvent` and the whole
  reserve/confirm/cancel-on-failure round trip through Inventory Service are gone.
  Payment/Notification needed **zero changes** for this; they already only cared about
  `OrderConfirmedEvent`, just now published one hop earlier.
- **Delivery**: each `OrderItem` snapshots the product's name/type/asset link at order time and
  generates an opaque `AccessKey` (`sk_live_...`, cosmetically Stripe-like — not a real credential
  for any live API). The key is only ever returned once the order is `Completed` (gated in
  `OrderDto`, not the database) — shown on the **My Products** page with reveal/copy, alongside
  the **Transactions** page (money) and the existing per-order Order Details view.
- **RBAC / Identity**: a new Identity Service (register/login/refresh/logout/me + admin
  user/role management) issues JWTs with one `permission` claim per grant
  (`ECommerce.BuildingBlocks.Auth.Permissions`). `Roles.Admin` gets every permission,
  `Roles.Customer` gets none (self-service endpoints just need to be authenticated). Every other
  service validates the same token (`AddJwtAuthentication`) and reads the caller's id off the
  token (`ClaimsPrincipal.GetUserId()`) instead of trusting a client-supplied id — closing the
  trust hole the old `X-Customer-Id` header left open. Registering publishes
  `UserRegisteredEvent`; Customer Service consumes it to create the linked `Customer` profile row
  with the same id, so "authenticated user" and "customer" stay one GUID everywhere downstream,
  same as before.
- **Admin console** (frontend, `/admin`, Admin-role-gated): add/edit catalog products, browse all
  orders, and grant roles to users — the `ProductsManage`/`OrdersManage`/`UsersManage`
  permissions enforce this server-side too, not just in the UI.

## Vertical Slice Architecture

Product, Customer, Order, Inventory, and Payment were restructured from layered Clean
Architecture (`Api` Controllers → `Application` service classes → `Infrastructure` repositories)
to Vertical Slice Architecture: each operation is a self-contained folder instead of a method on
a shared per-entity service.

```text
Product.Application/Features/CreateProduct/CreateProduct.cs
  └── public static class CreateProduct
        ├── record Command(...) : IRequest<ProductDto>      — the request
        ├── class Validator : AbstractValidator<Command>    — FluentValidation, runs in the pipeline
        └── class Handler : IRequestHandler<Command, ...>   — the entire use case, start to finish

Product.Api/Features/CreateProduct/CreateProductEndpoint.cs
  └── class CreateProductEndpoint : IEndpoint               — maps POST /api/products, calls IMediator.Send
```

- **`Domain` and the EF Core `DbContext`/repository stay shared per-service infrastructure** —
  a slice can't own the whole schema alone, so `IProductRepository`, `ProductDbContext`, etc.
  are still one shared seam every slice's Handler depends on. What moved into
  `Features/<Name>/` is the request-handling logic: what used to be
  `ProductService.CreateAsync(...)` is now `CreateProduct.Handler.Handle(...)`, in its own file,
  next to its own validator and its own request/response shape.
- **Minimal API + `IEndpoint`** replaces Controllers. Each `IEndpoint` implementation maps exactly
  one route; `AddEndpoints(assembly)`/`app.MapEndpoints()` (in `BuildingBlocks/WebApi`) discover
  and register them all by assembly scan, so adding a new slice never means editing a shared
  routing file. Cart and Notification still use Controllers — see "Architecture at a glance" above
  for why they were left out of this pass.
- **A hand-rolled mediator, not MediatR.** `BuildingBlocks/Application/Mediator` implements the
  same shape you'd recognize from MediatR (`IRequest<T>`, `IRequestHandler<,>`,
  `IPipelineBehavior<,>`, `IMediator.Send`) from scratch, with no external package — MediatR itself
  went commercial-only at v13 (July 2025), the same licensing shift that hit MassTransit at v9
  (see the comments in `Directory.Packages.props`). Rolling a ~150-line mediator sidesteps that
  entirely rather than pinning to an old free version and hoping it never needs a bump.
- **Two pipeline behaviors** wrap every request:
  - `ValidationBehaviour` runs every registered FluentValidation validator for the request before
    the handler executes — replacing controllers' old `validator.ValidateAndThrowAsync(...)` call.
  - `CachingBehaviour` is Redis cache-aside for any request implementing `ICacheableQuery`
    (`CacheKey` + `Expiration`) — currently only Product's `GetProductById`/`SearchProducts`
    opt in; everything else passes through untouched. This replaces the earlier
    `CachedProductService` decorator with the same cache-aside behavior expressed once, generically.
- **Internal (non-REST) operations are slices too.** The four operations MassTransit consumers
  trigger — `ConfirmOrderAfterReservation`, `CancelOrderAfterReservationFailure`,
  `CompleteOrderAfterPayment`, `MarkOrderPaymentFailed`, `ReserveInventoryForOrder`,
  `InitiatePaymentForOrder`, `HandleWebhook` — are ordinary slices with no `IEndpoint`; a consumer
  calls `IMediator.Send(...)` exactly like an HTTP endpoint would. They return
  `ECommerce.BuildingBlocks.Application.Mediator.Unit` (a tiny "no meaningful response" type)
  instead of a DTO.
- **Repository interfaces take primitives, not the old `*Query`/`*Request` DTOs** (e.g.
  `IProductRepository.SearchAsync(string? search, Guid? categoryId, ...)` instead of
  `SearchAsync(ProductQuery query, ...)`) — keeps Infrastructure from depending on any particular
  slice's request shape.
- **One shared `AppExceptionHandler`** (`BuildingBlocks/WebApi`) replaced five near-identical
  per-service copies; `ConcurrencyConflictException` moved from Inventory-local to
  `ECommerce.Contracts.Common` so the shared handler can map it to 409 for everyone.

> **Gotcha hit during this conversion**: naming an endpoint's own namespace the same as the
> feature it calls (`Product.Api.Features.SearchProducts` calling into
> `Product.Application.Features.SearchProducts.SearchProducts`) makes the bare type name resolve
> to the *namespace*, not the imported class — C# checks enclosing-namespace members before
> `using` directives. Every endpoint works around this with an import alias
> (`using Feature = Product.Application.Features.SearchProducts.SearchProducts;`) rather than a
> bare `using`.

## Running it

### Docker Compose (recommended)

Copy `.env.example` to `.env` (gitignored) and fill in:

- `JWT_SIGNING_KEY` — required for every service to start (validated to be ≥32 bytes at startup).
  Generate one with `openssl rand -base64 48` or similar.
- `ADMIN_SEED_EMAIL` / `ADMIN_SEED_PASSWORD` — bootstraps one Admin account on Identity Service's
  first startup so there's a way to log in as Admin. Idempotent (only ever creates, never
  updates) — safe to leave set across restarts.
- `RAZORPAY_KEY_ID` / `RAZORPAY_KEY_SECRET` — **test-mode** credentials from the
  [Razorpay Dashboard](https://dashboard.razorpay.com/) → Settings → API Keys. Everything else
  works without these; only "Pay Now" on a confirmed order needs real keys. See "Payments
  (Razorpay)" below for the full picture.

```bash
docker compose up -d --build
```

- Frontend: <http://localhost:5173>
- API Gateway: <http://localhost:5000> (`/health` for a liveness check)
- RabbitMQ management UI: <http://localhost:15672> (guest/guest)
- Per-service API docs (Development only, so via `dotnet run`, not through Compose/Gateway): each
  service exposes a [Scalar](https://github.com/scalar/scalar) UI at `/scalar` — e.g.
  <http://localhost:5001/scalar> for Product. See "Local dev without Docker" below for ports.

Every `.NET` service applies its EF Core migrations on startup, so the databases are created
automatically — no manual `dotnet ef database update` step needed for Compose.

`docker compose down` stops everything; add `-v` to also drop the SQL Server/Redis/RabbitMQ
volumes if you want a clean slate.

### Local dev without Docker

Requires SQL Server, Redis and RabbitMQ reachable at `localhost` (default ports), e.g. via
`docker compose up sqlserver redis rabbitmq -d`.

```bash
dotnet build ECommerce.slnx

# every service needs a signing key to start — export it once per terminal (or set it in your
# shell profile); this must match what Identity Service issues tokens with.
export Jwt__SigningKey=... # same value as JWT_SIGNING_KEY in .env

# apply migrations once per SQL-backed service, or let them auto-apply on first run
dotnet ef database update --project src/Services/Identity/Identity.Infrastructure --startup-project src/Services/Identity/Identity.Api
dotnet ef database update --project src/Services/Product/Product.Infrastructure --startup-project src/Services/Product/Product.Api
dotnet ef database update --project src/Services/Customer/Customer.Infrastructure --startup-project src/Services/Customer/Customer.Api
dotnet ef database update --project src/Services/Order/Order.Infrastructure --startup-project src/Services/Order/Order.Api
dotnet ef database update --project src/Services/Payment/Payment.Infrastructure --startup-project src/Services/Payment/Payment.Api

# run each service in its own terminal (ports 5000-5004, 5006-5008, see each Properties/launchSettings.json)
dotnet run --project src/Gateway/ECommerce.ApiGateway
dotnet run --project src/Services/Identity/Identity.Api
dotnet run --project src/Services/Product/Product.Api
dotnet run --project src/Services/Customer/Customer.Api
dotnet run --project src/Services/Cart/Cart.Api
dotnet run --project src/Services/Order/Order.Api

# Payment Service also needs Razorpay test-mode credentials or it will throw on startup (see
# "Payments (Razorpay)" below):
#   export Razorpay__KeyId=rzp_test_...
#   export Razorpay__KeySecret=...
dotnet run --project src/Services/Payment/Payment.Api

dotnet run --project src/Services/Notification/Notification.Api

cd src/Frontend/ecommerce-web && npm install && npm run dev
```

## What's been verified

**Before the digital-marketplace pivot** (physical-goods shape, Inventory Service still in the
loop), the full happy-path and failure-path order saga, the Vertical Slice Architecture
conversion, and Payment Service's Razorpay integration were all run live end-to-end against the
Docker Compose stack — including a real bug this caught (Minimal APIs read a different
`JsonOptions` instance than Controllers, so enums silently serialized as numbers until
`AddDefaultJsonOptions()` was added).

**The pivot itself (Identity/RBAC, the digital catalog, Inventory's removal, the redesigned
frontend) is build-verified, not yet live-verified**: `dotnet build ECommerce.slnx` succeeds with
zero errors, `dotnet ef migrations add` ran cleanly for every schema change, and `npm run
build`/`oxlint` pass on the frontend — but nobody has yet run `docker compose up -d --build` and
clicked through register → buy → see the delivered key → check the Admin console against a live
stack. That's the next thing to do; see "Verification" in the pivot's implementation notes, or
just: register, log in as the seeded Admin, add a product, buy it as a customer, confirm the
access key shows up on **My Products** and the payment on **Transactions**.

## Rate limiting

Implemented with ASP.NET Core's built-in `Microsoft.AspNetCore.RateLimiting` at the Gateway only
(§4 — one central place for cross-cutting concerns, no per-service limiting). Two fixed-window
policies, both partitioned per client IP (no auth yet, so IP is the best available key — see
`ClientKey()` in `ECommerce.ApiGateway/Program.cs`):

- **Global** (`RateLimiterOptions.GlobalLimiter`) — applies to every request through the Gateway.
  Generous by default (200 req/min/IP); tune via `RateLimiting:GlobalPermitLimit` /
  `GlobalWindowSeconds` in `appsettings.json`.
- **`writes`** — a stricter named policy (20 req/min/IP by default,
  `RateLimiting:WritesPermitLimit` / `WritesWindowSeconds`) opted into per-route via
  `RateLimiterPolicy: "writes"` on `orders-route` and `cart-route` in the YARP config, since those
  mutate state (order creation kicks off the whole saga). Both the global and the route policy
  apply together — the tighter one is what you'll actually hit.

Rejections return `429` with a `Retry-After` header and a small JSON body, not YARP's default
error page.

## Payments (Razorpay)

Payment Service (`src/Services/Payment`) integrates [Razorpay](https://razorpay.com/), joining
the order saga as a fourth downstream consumer alongside Inventory and Notification:

```text
Order Service                Payment Service                          Order/Notification Service
     │  OrderConfirmedEvent        │
     ├─────────────────────────────▶  creates Razorpay order, saves
     │                              │  a Payment row (Created)
     │                              │
     │            frontend: GET /api/payments/order/{orderId}
     │            → { razorpayKeyId, razorpayOrderId, amount, ... }
     │            → opens Razorpay Checkout in the browser
     │                              │
     │            frontend: POST /api/payments/verify
     │            (razorpay_order_id/payment_id/signature from Checkout's success handler)
     │                              │  verifies HMAC signature (Razorpay.Api.Utils),
     │                              │  marks Payment Succeeded/Failed
     │                              │
     │  ◀───────────────────────────┤  PaymentSucceededEvent / PaymentFailedEvent
     ├─ Completed / PaymentFailed   ├─ notifies the customer
```

- **One Payment row per order** (unique index on `OrderId`), created idempotently off
  `OrderConfirmedEvent`. A single Razorpay order can accept more than one payment attempt, so a
  `Failed` payment is retried in place
  rather than creating a new row; `OrderStatus.PaymentFailed` is not terminal — "Retry Payment"
  in the UI re-opens Checkout against the same Razorpay order id.
- **Signature verification** (`POST /api/payments/verify`) is the primary, always-available path
  — it needs nothing beyond the two API keys and works fully in local Docker Compose.
- **Webhook verification** (`POST /api/payments/webhook`, HMAC-SHA256 over the raw body against
  `Razorpay:WebhookSecret`) is a secondary, best-effort reconciliation path for when the browser
  never posts back to Verify (closed tab, dropped connection). It only receives calls if your
  Payment Service is reachable from the public internet — point a tool like `ngrok` at
  `payment-service`/the Gateway and register that URL + `payment.captured`/`payment.failed`
  events in the Razorpay Dashboard if you want to exercise it locally. Everything else in this
  README works without it.
- **Frontend**: `OrderDetailsPage` shows "Pay Now" once an order is `Confirmed`, dynamically
  loads Razorpay's `checkout.js`, and polls the order briefly after Checkout's success handler
  fires (Order Service completes the order asynchronously off `PaymentSucceededEvent`, so the
  verify response doesn't itself carry the final order status). Checkout itself shows every
  payment method enabled on the Razorpay account, including UPI — `config.display` in the
  Checkout options just makes UPI the first, prominent tab.
- **Headless UPI QR** (`POST /api/payments/order/{orderId}/upi-qr`, `GET .../upi-qr/status`) is a
  second, popup-free path next to Checkout: mints a single-use fixed-amount Razorpay QR Code
  entity (REST API, no SDK support for it — see `RazorpayGateway`'s plain `HttpClient` calls),
  shown via `UpiQrPanel`, which polls status every 2.5s since a QR scan has no client-side
  callback the way Checkout's `handler` provides one. Unlike Collect-flow UPI (VPA push requests),
  the QR Code API doesn't require separate Razorpay approval to use.

> **Gotcha for future consumers of a shared event**: MassTransit's default endpoint-name
> formatter derives the RabbitMQ queue name from the bare *consumer class name* — not its
> namespace or assembly. `OrderConfirmedEvent` is now consumed by both Notification and Payment;
> give their consumer classes distinct names (`PaymentOrderConfirmedConsumer` vs Notification's
> `OrderConfirmedConsumer`) or they'll silently collide onto one shared queue and compete for
> messages instead of each getting their own copy. Same reasoning applies to
> `PaymentSucceededEvent`/`PaymentFailedEvent`, each consumed by both Order and Notification.

## Known gaps (deliberately out of scope for this pass)

- **No transactional outbox.** Order Service saves the order, then publishes `OrderConfirmedEvent`
  as a second step. A crash between those two steps leaves the order stuck in `Pending` with no
  payment ever initiated. Add the outbox pattern (EF Core outbox table + MassTransit's built-in
  support) to close this.
- **SaaS "Monthly"/"Yearly" pricing is a label only** — there's no recurring billing engine or
  webhook behind `PricingModel`; every purchase is a single one-time payment regardless.
- **The generated `AccessKey` is a delivered credential, not a live one** — nothing gates real API
  traffic with it. This is a marketplace for keys, not an API gateway product.
- **No idempotent de-dup beyond Payment's unique-per-order row.** Order/Notification consumers
  don't guard against redelivered messages beyond what that unique index buys them.
- **No refund/cancellation-after-payment flow.** Cancelling an order that has already been paid
  doesn't trigger a Razorpay refund — Payment Service has no `POST /refund` yet.
- **Payment Service holds the Razorpay key secret and webhook secret in plain config/env vars**,
  fine for a learning project's test-mode keys but not how you'd hold live-mode secrets in
  production (a secrets manager, not `.env`, for those). The same applies to `Jwt:SigningKey`.
- **Admin bootstrap is a seeded account from `.env`**, not an invite/first-run wizard.
- **No pagination UI**, though the APIs support `page`/`pageSize` (Products, Orders, Payments).
- **Rate limiting / CORS is Gateway-only**, not per-service (intentional — see §4). Auth, by
  contrast, is deliberately per-service (see `AddJwtAuthentication`), not centralized at the
  Gateway.

## Project structure

Matches §15 of the spec:

```text
ECommerce/
├── src/
│   ├── Gateway/ECommerce.ApiGateway/       YARP routes in appsettings.json / appsettings.Docker.json
│   ├── Services/
│   │   ├── Identity/   Product/   Customer/   Order/   Payment/   → Api/Features, Application/Features,
│   │   │                                                             Domain, Infrastructure (see "Vertical
│   │   │                                                             Slice Architecture" above)
│   │   ├── Cart/Cart.Api/                  Redis-only, Controllers (not sliced — see above)
│   │   └── Notification/Notification.Api/  Redis + SignalR hub + MassTransit consumers, Controllers
│   ├── BuildingBlocks/
│   │   ├── Auth/                           Roles/Permissions/JwtOptions/AddJwtAuthentication — shared by every service
│   │   ├── Contracts/                      Integration events, PagedResult<T>, domain exceptions
│   │   ├── Application/                    Hand-rolled mediator + ValidationBehaviour/CachingBehaviour
│   │   └── WebApi/                         IEndpoint/AddEndpoints/MapEndpoints, shared AppExceptionHandler
│   └── Frontend/ecommerce-web/             React (Vite, JS) — see its own structure under src/
├── docker-compose.yml
└── ECommerce.slnx
```

Each Api project also has an `appsettings.Docker.json` that swaps `localhost` connection strings
for Docker service names — loaded automatically when `ASPNETCORE_ENVIRONMENT=Docker` (set in
`docker-compose.yml`).

### Centralized build settings and package versions

Two root-level MSBuild files remove per-project repetition across all 20 `.csproj` files:

- **`Directory.Build.props`** — `TargetFramework`, `Nullable`, `ImplicitUsings`, shared by every
  project (a project can still override locally if it ever needs to).
- **`Directory.Packages.props`** — Central Package Management (`ManagePackageVersionsCentrally`).
  Every `<PackageReference>` in the solution omits `Version` and resolves it from here. **To add
  or bump a package, edit `Directory.Packages.props` only**, then `<PackageReference Include="X" />`
  (no version) in the project that needs it.

Because of this, each service's Dockerfile explicitly `COPY`s both root props files before its
first `dotnet restore` layer — MSBuild walks up from the `.csproj` to find them, so they have to
be present at that point in the build or restore fails.

## Extending this

The natural next steps, in the order they'd have the most impact:

1. Live-verify the pivot end-to-end against Docker Compose (see "What's been verified") — it's
   build-verified but nobody has clicked through it live yet.
2. Close the outbox gap in Order Service.
3. Add integration tests around the order→payment flow (it's the part most worth protecting from
   regressions).
4. A real refund flow for cancelling a paid order (`POST /api/payments/refund`).
5. Actual recurring billing for `PricingModel.Monthly`/`Yearly` SaaS products, if that's worth the
   complexity for a learning project — today it's a label with no automation behind it.
