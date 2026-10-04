import React from 'react';
import { Card, CardActionArea, Typography } from '@mui/material';
import AvatarImage from 'src/components/kids/AvatarImage';

interface IProps {
    name: string;
    avatarId?: string;
    disabled?: boolean;
    testId?: string;
    onSelect: () => void;
}

/** Profile choice tile: same look as ChoiceTile, but a button that signs the child in. */
const ProfileTile: React.FC<IProps> = (props) => {
    const { name, avatarId, disabled, testId, onSelect } = props;

    return (
        <Card sx={{ height: '100%' }}>
            <CardActionArea
                disabled={disabled}
                data-testid={testId}
                onClick={onSelect}
                sx={{
                    height: '100%',
                    minHeight: 160,
                    p: 2,
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    justifyContent: 'center',
                    gap: 1.5,
                    textAlign: 'center',
                    '&.Mui-focusVisible, &:focus-visible': {
                        outline: '3px solid',
                        outlineColor: 'primary.main',
                        outlineOffset: -3,
                    },
                }}
            >
                <AvatarImage avatarId={avatarId} name={name} size={96} />
                <Typography variant="h6" component="span" sx={{ fontWeight: 700 }}>
                    {name}
                </Typography>
            </CardActionArea>
        </Card>
    );
};

export default ProfileTile;
