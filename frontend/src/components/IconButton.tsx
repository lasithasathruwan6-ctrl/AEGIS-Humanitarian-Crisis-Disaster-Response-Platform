"use client";

import type { ButtonHTMLAttributes, ReactNode } from "react";

interface IconButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  label: string;
  icon: ReactNode;
}

export function IconButton({
  label,
  icon,
  className = "",
  type = "button",
  ...rest
}: IconButtonProps) {
  return (
    <button
      type={type}
      aria-label={label}
      title={label}
      className={`btn btn-secondary btn-icon ${className}`.trim()}
      {...rest}
    >
      <span aria-hidden="true">{icon}</span>
    </button>
  );
}
