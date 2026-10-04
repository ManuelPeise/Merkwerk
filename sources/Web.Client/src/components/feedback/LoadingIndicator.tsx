import React from 'react';
import { Box, CircularProgress } from '@mui/material';
import { useTranslation } from 'src/hooks/useTranslation';

interface IProps {
    uiTestId: string;
}

const LoadingIndicator: React.FC<IProps> = (props) => {
    const { uiTestId } = props;
    const { getResource } = useTranslation();

    return (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }} data-testid={uiTestId}>
            <CircularProgress aria-label={getResource('labelLoading')} />
        </Box>
    );
};

export default LoadingIndicator;
