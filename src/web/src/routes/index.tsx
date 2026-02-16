import { createBrowserRouter } from 'react-router-dom';
import { SignIn, SignUp } from '@clerk/clerk-react';
import ProtectedRoute from '../components/auth/ProtectedRoute';
import VolunteerRegisterPage from '../pages/VolunteerRegisterPage';
import AdminLayout from '../components/admin/AdminLayout';
import AdminOrganizationsPage from '../pages/AdminOrganizationsPage';
import AdminOrgDetailPage from '../pages/AdminOrgDetailPage';

// Placeholder page components
function PlaceholderPage({ title }: { title: string }) {
  return (
    <div className="flex items-center justify-center min-h-screen bg-cream">
      <h1 className="font-display text-3xl text-navy">{title}</h1>
    </div>
  );
}

function AuthPage({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex items-center justify-center min-h-screen bg-cream">{children}</div>
  );
}

export const router = createBrowserRouter([
  // Public routes
  { path: '/', element: <PlaceholderPage title="Feed" /> },
  { path: '/explore', element: <PlaceholderPage title="Explore" /> },
  { path: '/org/:id', element: <PlaceholderPage title="Organization" /> },

  // Auth routes
  {
    path: '/login',
    element: (
      <AuthPage>
        <SignIn routing="path" path="/login" signUpUrl="/register/volunteer" />
      </AuthPage>
    ),
  },
  {
    path: '/register/volunteer',
    element: <VolunteerRegisterPage />,
  },
  {
    path: '/register/organization',
    element: (
      <AuthPage>
        <SignUp routing="path" path="/register/organization" signInUrl="/login" />
      </AuthPage>
    ),
  },

  // Protected routes
  {
    element: <ProtectedRoute />,
    children: [
      { path: '/saved', element: <PlaceholderPage title="Saved Posts" /> },
      { path: '/profile', element: <PlaceholderPage title="My Profile" /> },
      { path: '/org/dashboard', element: <PlaceholderPage title="Org Dashboard" /> },
      { path: '/org/create', element: <PlaceholderPage title="Create Post" /> },
      { path: '/org/content', element: <PlaceholderPage title="Manage Content" /> },
    ],
  },

  // Admin routes
  {
    element: <ProtectedRoute />,
    children: [
      {
        element: <AdminLayout />,
        children: [
          { path: '/admin/organizations', element: <AdminOrganizationsPage /> },
          { path: '/admin/organizations/:id', element: <AdminOrgDetailPage /> },
        ],
      },
    ],
  },
]);
