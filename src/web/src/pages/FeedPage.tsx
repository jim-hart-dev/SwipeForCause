import BottomNav from '../components/shared/BottomNav';
import FeedContainer from '../components/feed/FeedContainer';

export default function FeedPage() {
  return (
    <div className="h-screen bg-navy flex flex-col">
      <FeedContainer />
      <BottomNav />
    </div>
  );
}
