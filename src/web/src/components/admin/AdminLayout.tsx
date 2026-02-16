import { Link, Outlet } from 'react-router-dom';

export default function AdminLayout() {
  return (
    <div className="min-h-screen bg-white font-body">
      <nav className="border-b border-gray-200 bg-navy px-6 py-4">
        <div className="mx-auto flex max-w-7xl items-center justify-between">
          <Link to="/admin/organizations" className="font-display text-xl text-white">
            SwipeForCause Admin
          </Link>
          <div className="flex items-center gap-6">
            <Link
              to="/admin/organizations"
              className="text-sm font-medium text-gray-300 hover:text-white"
            >
              Organizations
            </Link>
            <Link to="/" className="text-sm text-gray-400 hover:text-white">
              Back to App
            </Link>
          </div>
        </div>
      </nav>
      <main className="mx-auto max-w-7xl px-6 py-8">
        <Outlet />
      </main>
    </div>
  );
}
