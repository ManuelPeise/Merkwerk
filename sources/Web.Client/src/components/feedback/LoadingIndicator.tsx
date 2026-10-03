import React from 'react';
import { Box, CircularProgress } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';

const LoadingIndicator: React.FC = () => {
    const { getResource } = useTranslation();

    return (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
            <CircularProgress aria-label={getResource('labelLoading')} />
        </Box>
    );
};

export default LoadingIndicator;
