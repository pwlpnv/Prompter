import { useState } from "react";

interface Props {
  onSubmit: (prompts: string[]) => Promise<void>;
}

export function PromptForm({ onSubmit }: Props) {
  const [text, setText] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const prompts = text
      .split("\n")
      .map((l) => l.trim())
      .filter((l) => l.length > 0);
    if (prompts.length === 0) return;
    setSubmitting(true);
    try {
      await onSubmit(prompts);
      setText("");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <textarea
        value={text}
        onChange={(e) => setText(e.target.value)}
        placeholder="Enter prompts, one per line..."
        rows={5}
        style={{ width: "100%", fontFamily: "inherit" }}
      />
      <button type="submit" disabled={submitting}>
        {submitting ? "Submitting..." : "Submit Prompts"}
      </button>
    </form>
  );
}
