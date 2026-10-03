import React from 'react';
import { Avatar } from '@mui/material';
import { avatarImages, type AvatarId } from 'src/assets/avatars/avatars';

interface IProps {
    /** One of the built-in avatar ids; unknown or missing ids fall back to the initial of the name. */
    avatarId?: string;
    /** First name, used for the fallback initial. */
    name: string;
    /** Diameter in px. */
    size: number;
}

const isAvatarId = (value: string | undefined): value is AvatarId =>
    value !== undefined && value in avatarImages;

/** Decorative: the name is always shown next to the avatar, so the image has no alt text. */
const AvatarImage: React.FC<IProps> = (props) => {
    const { avatarId, name, size } = props;

    return (
        <Avatar
            src={isAvatarId(avatarId) ? avatarImages[avatarId] : undefined}
            alt=""
            sx={{ width: size, height: size, bgcolor: 'primary.main', fontSize: size / 2 }}
        >
            {name.charAt(0).toUpperCase()}
        </Avatar>
    );
};

export default AvatarImage;
