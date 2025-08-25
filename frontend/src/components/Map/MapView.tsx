import { useEffect, useRef, useState } from react;
import Map from ol/Map;
import View from ol/View;
import TileLayer from ol/layer/Tile;
import OSM from ol/source/OSM;
import VectorSource from ol/source/Vector;
import VectorLayer from ol/layer/Vector;
import { fromLonLat } from ol/proj;
import Draw from ol/interaction/Draw;
import Modify from ol/interaction/Modify;
import Select from ol/interaction/Select;
import WKT from ol/format/WKT;
import { createGeometry as createGeometryApi } from ../../api/geometry;
import type { GeometryDto } from ../../types/api;

export default function MapView({ mode }: { mode: Select | Point | LineString | Polygon }) {
  const mapDivRef = useRef<HTMLDivElement | null>(null);
  const vectorSourceRef = useRef(new VectorSource());
  const [map, setMap] = useState<Map | null>(null);
  const drawRef = useRef<Draw | null>(null);
  const modifyRef = useRef<Modify | null>(null);
  const selectRef = useRef<Select | null>(null);

  useEffect(() => {
    const base = new TileLayer({ source: new OSM() });
    const vector = new VectorLayer({ source: vectorSourceRef.current });

    const mapInstance = new Map({
      target: mapDivRef.current!,
      layers: [base, vector],
      view: new View({ center: fromLonLat([29.0, 41.0]), zoom: 6 }),
    });

    setMap(mapInstance);

    return () => {
      mapInstance.setTarget(undefined);
    };
  }, []);

  useEffect(() => {
    if (!map) return;

    if (drawRef.current) { map.removeInteraction(drawRef.current); drawRef.current = null; }
    if (modifyRef.current) { map.removeInteraction(modifyRef.current); modifyRef.current = null; }
    if (selectRef.current) { map.removeInteraction(selectRef.current); selectRef.current = null; }

    const vectorSource = vectorSourceRef.current;

    if (mode === Select) {
      const select = new Select();
      selectRef.current = select;
      map.addInteraction(select);
      return;
    }

    const draw = new Draw({ source: vectorSource as any, type: mode as any });
    drawRef.current = draw;

    draw.on(drawend, async (evt) => {
      const feature = evt.feature as any;
      const wkt = new WKT().writeFeature(feature);

      const payload: GeometryDto = {
        name: Yeni
