export interface PromptDetails {
  id: number;
  prompt: string;
  status: "Pending" | "Processing" | "Completed" | "Failed";
  response: string | null;
  createdAt: string;
  completedAt: string | null;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
}
