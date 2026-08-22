import { useEffect, useState } from 'react';
import { assignRole, listUsers } from '../../services/authApi';
import { useToast } from '../../hooks/useToast';

const ROLES = ['Customer', 'Admin'];

export function AdminUsersPage() {
  const { addToast } = useToast();
  const [result, setResult] = useState({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(true);
  const [busyId, setBusyId] = useState(null);

  function load() {
    setLoading(true);
    listUsers({ pageSize: 100 }).then(setResult).finally(() => setLoading(false));
  }

  useEffect(load, []);

  async function handleAssign(user, role) {
    if (user.roles.includes(role)) return;
    setBusyId(user.id);
    try {
      await assignRole(user.id, role);
      addToast(`Granted ${role} to ${user.email}`, 'success');
      load();
    } catch {
      addToast('Could not update this user’s role.', 'info');
    } finally {
      setBusyId(null);
    }
  }

  return (
    <div>
      <div className="page-header-row">
        <div>
          <h1>Users</h1>
          <p className="page-subtitle">Everyone with an account, and their assigned roles.</p>
        </div>
      </div>

      {loading && <p>Loading...</p>}

      {!loading && (
        <div className="data-table-wrap">
          <table className="data-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Roles</th>
                <th>Joined</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {result.items.map((user) => (
                <tr key={user.id}>
                  <td>{user.fullName}</td>
                  <td className="mono">{user.email}</td>
                  <td>{user.roles.map((r) => <span key={r} className="role-pill" style={{ marginLeft: 0, marginRight: 6 }}>{r}</span>)}</td>
                  <td>{new Date(user.createdAt).toLocaleDateString()}</td>
                  <td className="table-actions">
                    {ROLES.filter((r) => !user.roles.includes(r)).map((r) => (
                      <button key={r} className="link-button" disabled={busyId === user.id} onClick={() => handleAssign(user, r)}>
                        Make {r}
                      </button>
                    ))}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
