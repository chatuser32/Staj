export type ResponseWrapper<T> = {
  success: boolean;
  message: string;
  data: T;
};

export type GeometryDto = {
  name: string;
  wkt: string;
  type: "Point" | "LineString" | "Polygon";
};
