import { useQuery } from '@tanstack/react-query';
import { listGeometries } from '../../api/geometry';

export default function LocationsList() {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['geometries'],
    queryFn: listGeometries,
  });

  if (isLoading) return <div>Yükleniyor...</div>;
  if (isError || !data?.success) return <div>Hata</div>;

  return (
    <div>
      <h4>Geometriler</h4>
      <ul>
        {data.data.map((g, idx) => (
          <li key={idx}>{g.name} - {g.type}</li>
        ))}
      </ul>
    </div>
  );
}
