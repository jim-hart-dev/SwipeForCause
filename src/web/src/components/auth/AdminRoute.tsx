import { useAuth, useUser } from '@clerk/clerk-react';
import { Navigate, Outlet } from 'react-router-dom';

export default function AdminRoute() {
  const { isLoaded, isSignedIn } = useAuth();
  const { user } = useUser();

  if (!isLoaded) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-cream">
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-coral border-t-transparent" />
      </div>
    );
  }

  if (!isSignedIn) {
    return <Navigate to="/login" replace />;
  }

  const userType = (user?.publicMetadata as { user_type?: string })?.user_type;
  if (userType !== 'admin') {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
