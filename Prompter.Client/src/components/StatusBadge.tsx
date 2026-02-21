interface Props {
  status: string;
}

const colors: Record<string, string> = {
  Pending: "#888",
  Processing: "#e6a817",
  Completed: "#2e7d32",
  Failed: "#c62828",
};

export function StatusBadge({ status }: Props) {
  return (
    <span
      style={{
        display: "inline-block",
        padding: "2px 8px",
        borderRadius: "4px",
        fontSize: "0.8rem",
        color: "white",
        backgroundColor: colors[status] ?? "#666",
      }}
    >
      {status}
    </span>
  );
}
