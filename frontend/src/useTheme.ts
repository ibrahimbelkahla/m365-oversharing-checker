import { useEffect, useState } from 'react';

type Theme = 'light' | 'dark';
const STORAGE_KEY = 'theme';

function initialTheme(): Theme {
  const saved = localStorage.getItem(STORAGE_KEY);
  if (saved === 'light' || saved === 'dark') return saved;
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

/** Light/dark mode: follows the OS by default, remembers the user's choice. */
export function useTheme() {
  const [theme, setTheme] = useState<Theme>(initialTheme);

  useEffect(() => {
    document.documentElement.dataset.theme = theme;
  }, [theme]);

  const toggleTheme = () => {
    const next = theme === 'dark' ? 'light' : 'dark';
    localStorage.setItem(STORAGE_KEY, next);
    setTheme(next);
  };

  return { theme, toggleTheme };
}
