import React from 'react';
import { Typography } from '@mui/material';

type CaptionProps = {
    caption: string;
};

const Caption: React.FC<CaptionProps> = (props) => {
    const { caption } = props;

    return <Typography variant="h3">{caption}</Typography>;
};

export default Caption;
