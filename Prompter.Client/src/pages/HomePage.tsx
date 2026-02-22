import { useEffect, useState } from "react";
import { fetchPromptsPaged, createPrompts } from "../api";
import type { PromptDetails } from "../types";
import { PromptForm } from "../components/PromptForm";
import { PromptList } from "../components/PromptList";

const POLL_INTERVAL = 4000;
const RECENT_COUNT = 20;

export function HomePage() {
  const [prompts, setPrompts] = useState<PromptDetails[]>([]);
  const [error, setError] = useState<string | null>(null);

  const loadPrompts = async () => {
    try {
      const data = await fetchPromptsPaged(1, RECENT_COUNT);
      setPrompts(data.items);
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load prompts");
    }
  };

  useEffect(() => {
    loadPrompts();
    const interval = setInterval(loadPrompts, POLL_INTERVAL);
    return () => clearInterval(interval);
  }, []);

  const handleSubmit = async (texts: string[]) => {
    await createPrompts(texts);
    await loadPrompts();
  };

  return (
    <>
      <h1>JustPromptIT</h1>
      <PromptForm onSubmit={handleSubmit} />
      {error && <p style={{ color: "red" }}>{error}</p>}
      <h2>Recent Prompts</h2>
      <PromptList prompts={prompts} />
    </>
  );
}
