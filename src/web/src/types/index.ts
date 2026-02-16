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

export interface Volunteer {
  volunteerId: string;
  displayName: string;
}

export interface RegisterVolunteerRequest {
  displayName: string;
  city: string;
  state: string;
  categoryIds: string[];
}

export interface RegisterVolunteerResponse {
  volunteerId: string;
  displayName: string;
}

export interface AdminOrganization {
  id: string;
  name: string;
  ein: string;
  contactEmail: string;
  websiteUrl: string | null;
  city: string | null;
  state: string | null;
  verificationStatus: string;
  createdAt: string;
  logoUrl: string | null;
}

export interface AdminOrganizationDetail {
  id: string;
  name: string;
  ein: string;
  description: string;
  contactName: string;
  contactEmail: string;
  websiteUrl: string | null;
  city: string | null;
  state: string | null;
  verificationStatus: string;
  verifiedAt: string | null;
  createdAt: string;
  logoUrl: string | null;
  coverImageUrl: string | null;
  categories: AdminCategory[];
}

export interface AdminCategory {
  id: string;
  name: string;
  slug: string;
}

export interface VerifyOrganizationRequest {
  status: 'verified' | 'rejected';
  reason?: string;
}

export interface VerifyOrganizationResponse {
  id: string;
  name: string;
  verificationStatus: string;
  verifiedAt: string | null;
  updatedAt: string;
}
