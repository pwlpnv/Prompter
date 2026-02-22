import { useState, useEffect, useRef, useCallback } from "react";
import { fetchPromptsPaged } from "../api";
import type { PromptDetails } from "../types";
import { PromptList } from "../components/PromptList";

const PAGE_SIZE = 20;

export function AllPromptsPage() {
  const [prompts, setPrompts] = useState<PromptDetails[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const sentinelRef = useRef<HTMLDivElement | null>(null);
  const pageRef = useRef(1);

  const hasMore = prompts.length < totalCount;

  const loadPage = useCallback(async (pageNum: number) => {
    setLoading(true);
    try {
      const data = await fetchPromptsPaged(pageNum, PAGE_SIZE);
      setPrompts((prev) =>
        pageNum === 1 ? data.items : [...prev, ...data.items]
      );
      setTotalCount(data.totalCount);
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Failed to load prompts");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadPage(1);
  }, [loadPage]);

  useEffect(() => {
    if (!sentinelRef.current) return;
    const observer = new IntersectionObserver(
      (entries) => {
        if (entries[0].isIntersecting && hasMore && !loading) {
          pageRef.current += 1;
          loadPage(pageRef.current);
        }
      },
      { threshold: 0.1 }
    );
    observer.observe(sentinelRef.current);
    return () => observer.disconnect();
  }, [hasMore, loading, loadPage]);

  return (
    <>
      <h2>All Prompts ({totalCount})</h2>
      {error && <p style={{ color: "red" }}>{error}</p>}
      <PromptList prompts={prompts} />
      {hasMore && <div ref={sentinelRef} style={{ height: 1 }} />}
      {loading && <p>Loading...</p>}
      {!hasMore && prompts.length > 0 && <p>All prompts loaded.</p>}
    </>
  );
}
