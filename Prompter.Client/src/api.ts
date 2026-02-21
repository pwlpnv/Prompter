import type { PromptDetails } from "./types";

const API_BASE = import.meta.env.VITE_API_URL ?? "";

export async function fetchPrompts(): Promise<PromptDetails[]> {
  const res = await fetch(`${API_BASE}/api/prompts`);
  if (!res.ok) throw new Error(`GET /api/prompts failed: ${res.status}`);
  return res.json();
}

export async function createPrompts(prompts: string[]): Promise<PromptDetails[]> {
  const res = await fetch(`${API_BASE}/api/prompts`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ prompts }),
  });
  if (!res.ok) throw new Error(`POST /api/prompts failed: ${res.status}`);
  return res.json();
}
