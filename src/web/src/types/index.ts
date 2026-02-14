export interface PagedResponse<T> {
  data: T[];
  cursor: string | null;
  hasMore: boolean;
}

export interface ErrorResponse {
  error: {
    code: string;
    message: string;
    details?: unknown;
  };
}

export interface Organization {
  organizationId: string;
  name: string;
  description: string;
  logoUrl: string | null;
  coverImageUrl: string | null;
  city: string | null;
  state: string | null;
  isVerified: boolean;
  followerCount: number;
}

export interface Post {
  postId: string;
  title: string;
  description: string | null;
  mediaType: 'video' | 'image';
  organization: Organization;
  createdAt: string;
}

export interface Opportunity {
  opportunityId: string;
  title: string;
  description: string;
  scheduleType: 'one_time' | 'recurring' | 'flexible';
  startDate: string | null;
  endDate: string | null;
  location: string | null;
  isRemote: boolean;
  timeCommitment: string | null;
}

export interface Category {
  categoryId: string;
  name: string;
  slug: string;
  icon: string | null;
}
