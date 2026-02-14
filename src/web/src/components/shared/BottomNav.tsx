import { NavLink } from 'react-router-dom';

const navItems = [
  { to: '/', label: 'Feed' },
  { to: '/explore', label: 'Explore' },
  { to: '/saved', label: 'Saved' },
  { to: '/profile', label: 'Profile' },
];

export default function BottomNav() {
  return (
    <nav className="fixed bottom-0 left-0 right-0 bg-white border-t border-gray-200 px-4 py-2">
      <ul className="flex justify-around">
        {navItems.map((item) => (
          <li key={item.to}>
            <NavLink
              to={item.to}
              className={({ isActive }) =>
                `flex flex-col items-center text-xs font-body ${
                  isActive ? 'text-coral' : 'text-navy/60'
                }`
              }
            >
              <span>{item.label}</span>
            </NavLink>
          </li>
        ))}
      </ul>
    </nav>
  );
}
