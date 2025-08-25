import { useState } from 'react';
import { CssBaseline, ThemeProvider, createTheme, Box, Stack, Button, Divider } from '@mui/material';
import MapView from './components/Map/MapView';
import LocationsList from './components/Locations/LocationsList';

type DrawMode = 'Select' | 'Point' | 'LineString' | 'Polygon';

const theme = createTheme();

function App() {
  const [mode, setMode] = useState<DrawMode>('Select');

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Stack direction="row" style={{ height: '100vh' }}>
        <Box style={{ width: 320, borderRight: '1px solid #ddd', padding: 16, overflow: 'auto' }}>
          <Stack spacing={1}>
            <Button variant={mode==='Select'?'contained':'outlined'} onClick={() => setMode('Select')}>Select</Button>
            <Button variant={mode==='Point'?'contained':'outlined'} onClick={() => setMode('Point')}>Point</Button>
            <Button variant={mode==='LineString'?'contained':'outlined'} onClick={() => setMode('LineString')}>LineString</Button>
            <Button variant={mode==='Polygon'?'contained':'outlined'} onClick={() => setMode('Polygon')}>Polygon</Button>
            <Divider />
            <LocationsList />
          </Stack>
        </Box>
        <Box style={{ flex: 1 }}>
          <MapView mode={mode} />
        </Box>
      </Stack>
    </ThemeProvider>
  );
}

export default App;
