import { useEffect, useState } from "react";
import { fetchPrompts, createPrompts } from "./api";
import type { PromptDetails } from "./types";
import { PromptForm } from "./components/PromptForm";
import { PromptList } from "./components/PromptList";
import "./App.css";

const POLL_INTERVAL = 4000;

function App() {
  const [prompts, setPrompts] = useState<PromptDetails[]>([]);
  const [error, setError] = useState<string | null>(null);

  const loadPrompts = async () => {
    try {
      const data = await fetchPrompts();
      setPrompts(data);
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
    <div className="container">
      <h1>Prompter</h1>
      <PromptForm onSubmit={handleSubmit} />
      {error && <p style={{ color: "red" }}>{error}</p>}
      <h2>All Prompts</h2>
      <PromptList prompts={prompts} />
    </div>
  );
}

export default App;
