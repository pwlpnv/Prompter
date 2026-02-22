import { useState } from "react";

interface Props {
  onSubmit: (prompts: string[]) => Promise<void>;
}

export function PromptForm({ onSubmit }: Props) {
  const [mode, setMode] = useState<"single" | "batch">("single");
  const [text, setText] = useState("");
  const [batch, setBatch] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);

  const handleSingleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const trimmed = text.trim();
    if (trimmed.length === 0) return;
    setSubmitting(true);
    try {
      await onSubmit([trimmed]);
      setText("");
    } finally {
      setSubmitting(false);
    }
  };

  const handleAddToBatch = () => {
    const trimmed = text.trim();
    if (trimmed.length === 0) return;
    setBatch((prev) => [...prev, trimmed]);
    setText("");
  };

  const handleRemoveFromBatch = (index: number) => {
    setBatch((prev) => prev.filter((_, i) => i !== index));
  };

  const handleSendBatch = async () => {
    if (batch.length === 0) return;
    setSubmitting(true);
    try {
      await onSubmit(batch);
      setBatch([]);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <div className="mode-toggle">
        <button
          type="button"
          className={mode === "single" ? "active" : ""}
          onClick={() => setMode("single")}
        >
          Single Prompt
        </button>
        <button
          type="button"
          className={mode === "batch" ? "active" : ""}
          onClick={() => setMode("batch")}
        >
          Batch
        </button>
      </div>

      {mode === "single" ? (
        <form onSubmit={handleSingleSubmit}>
          <textarea
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder="Enter your prompt..."
            rows={5}
            style={{ width: "100%", fontFamily: "inherit" }}
          />
          <button type="submit" disabled={submitting}>
            {submitting ? "Submitting..." : "Submit"}
          </button>
        </form>
      ) : (
        <div>
          <textarea
            value={text}
            onChange={(e) => setText(e.target.value)}
            placeholder="Type one prompt at a time..."
            rows={5}
            style={{ width: "100%", fontFamily: "inherit" }}
          />
          <div style={{ display: "flex", gap: "8px" }}>
            <button type="button" onClick={handleAddToBatch} disabled={submitting}>
              Add to Batch
            </button>
            <button
              type="button"
              onClick={handleSendBatch}
              disabled={submitting || batch.length === 0}
            >
              {submitting ? "Sending..." : `Send Batch (${batch.length})`}
            </button>
          </div>

          {batch.length > 0 && (
            <ul className="batch-preview">
              {batch.map((item, index) => (
                <li key={index}>
                  <span className="batch-item-text">
                    {item.length > 100 ? item.substring(0, 100) + "..." : item}
                  </span>
                  <button
                    type="button"
                    className="remove-btn"
                    onClick={() => handleRemoveFromBatch(index)}
                  >
                    x
                  </button>
                </li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  );
}
