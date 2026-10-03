import React from 'react';
import { Link as MuiLink } from '@mui/material';
import { Link } from 'react-router-dom';

interface IProps {
    label: string;
    to: string;
}

/** Text link inside forms, e.g. "Forgot password?". */
const FormLink: React.FC<IProps> = (props) => {
    const { label, to } = props;

    return (
        <MuiLink
            component={Link}
            to={to}
            sx={{
                alignSelf: 'flex-start',
                display: 'inline-flex',
                alignItems: 'center',
                minHeight: 48,
            }}
        >
            {label}
        </MuiLink>
    );
};

export default FormLink;
