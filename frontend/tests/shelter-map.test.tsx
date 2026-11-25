import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ShelterMap } from "@/components/ShelterMap";

describe("ShelterMap tile failure handling", () => {
  it("displays a clear alert message and retry button when map tiles fail to load", () => {
    render(
      <ShelterMap
        shelters={[
          { id: "s1", name: "Central School", district: "Colombo", capacity: 120, occupied: 45 },
        ]}
      />
    );

    const tileImg = screen.getByRole("img", {
      name: /regional map tiles/i,
    });
    fireEvent.error(tileImg);

    const alert = screen.getByRole("alert");
    expect(alert).toHaveTextContent("Map tiles could not be loaded.");
    expect(alert).toHaveTextContent("shelter list below");

    fireEvent.click(screen.getByRole("button", { name: "Retry loading map" }));
    expect(
      screen.getByRole("img", { name: /regional map tiles/i })
    ).toBeInTheDocument();
  });
});
