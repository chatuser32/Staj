import httpClient from './httpClient';
import type { ResponseWrapper, GeometryDto } from '../types/api';

export async function listGeometries() {
  const res = await httpClient.get<ResponseWrapper<GeometryDto[]>>('/api/geometry');
  return res.data;
}

export async function getGeometry(id: number) {
  const res = await httpClient.get<ResponseWrapper<GeometryDto>>(`/api/geometry/${id}`);
  return res.data;
}

export async function createGeometry(payload: GeometryDto) {
  const res = await httpClient.post<ResponseWrapper<GeometryDto>>('/api/geometry', payload);
  return res.data;
}

export async function updateGeometry(id: number, payload: GeometryDto) {
  const res = await httpClient.put<ResponseWrapper<GeometryDto>>(`/api/geometry/${id}`, payload);
  return res.data;
}

export async function deleteGeometry(id: number) {
  const res = await httpClient.delete<ResponseWrapper<boolean>>(`/api/geometry/${id}`);
  return res.data;
}
