import { createBrowserRouter } from 'react-router-dom';

// Placeholder page components
function PlaceholderPage({ title }: { title: string }) {
  return (
    <div className="flex items-center justify-center min-h-screen bg-cream">
      <h1 className="font-display text-3xl text-navy">{title}</h1>
    </div>
  );
}

export const router = createBrowserRouter([
  { path: '/', element: <PlaceholderPage title="Feed" /> },
  { path: '/explore', element: <PlaceholderPage title="Explore" /> },
  { path: '/saved', element: <PlaceholderPage title="Saved Posts" /> },
  { path: '/profile', element: <PlaceholderPage title="My Profile" /> },
  { path: '/org/dashboard', element: <PlaceholderPage title="Org Dashboard" /> },
  { path: '/org/create', element: <PlaceholderPage title="Create Post" /> },
  { path: '/org/content', element: <PlaceholderPage title="Manage Content" /> },
  { path: '/org/:id', element: <PlaceholderPage title="Organization" /> },
  { path: '/login', element: <PlaceholderPage title="Login" /> },
  { path: '/register/volunteer', element: <PlaceholderPage title="Register as Volunteer" /> },
  {
    path: '/register/organization',
    element: <PlaceholderPage title="Register Organization" />,
  },
]);
