import axios from "axios";

const base = (import.meta.env.VITE_API_BASE_URL as string | undefined) ?? "/";

export const httpClient = axios.create({
  baseURL: base,
  headers: { "Content-Type": "application/json" },
});

httpClient.interceptors.response.use(
  (response) => response,
  (error) => Promise.reject(error)
);

export default httpClient;
