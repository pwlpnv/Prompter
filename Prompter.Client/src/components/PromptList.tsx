import type { PromptDetails } from "../types";
import { StatusBadge } from "./StatusBadge";

interface Props {
  prompts: PromptDetails[];
}

export function PromptList({ prompts }: Props) {
  if (prompts.length === 0) {
    return <p>No prompts yet. Submit some above.</p>;
  }

  return (
    <table>
      <thead>
        <tr>
          <th>ID</th>
          <th>Prompt</th>
          <th>Status</th>
          <th>Response</th>
          <th>Created</th>
        </tr>
      </thead>
      <tbody>
        {prompts.map((p) => (
          <tr key={p.id}>
            <td>{p.id}</td>
            <td>{p.prompt}</td>
            <td>
              <StatusBadge status={p.status} />
            </td>
            <td style={{ maxWidth: 400, wordBreak: "break-word" }}>
              {p.response ?? "---"}
            </td>
            <td>{new Date(p.createdAt).toLocaleString()}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
