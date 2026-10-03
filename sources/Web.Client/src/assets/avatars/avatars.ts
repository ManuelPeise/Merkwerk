import fox from 'src/assets/avatars/fox.svg';
import owl from 'src/assets/avatars/owl.svg';
import cat from 'src/assets/avatars/cat.svg';
import dog from 'src/assets/avatars/dog.svg';
import bear from 'src/assets/avatars/bear.svg';
import rabbit from 'src/assets/avatars/rabbit.svg';
import frog from 'src/assets/avatars/frog.svg';
import lion from 'src/assets/avatars/lion.svg';
import panda from 'src/assets/avatars/panda.svg';
import pig from 'src/assets/avatars/pig.svg';
import mouse from 'src/assets/avatars/mouse.svg';
import penguin from 'src/assets/avatars/penguin.svg';

/** Fixed set of built-in avatars. The backend validates avatar ids against the same list (LP-105). */
export const avatarIds = [
    'fox',
    'owl',
    'cat',
    'dog',
    'bear',
    'rabbit',
    'frog',
    'lion',
    'panda',
    'pig',
    'mouse',
    'penguin',
] as const;

export type AvatarId = (typeof avatarIds)[number];

export const avatarImages: Record<AvatarId, string> = {
    fox,
    owl,
    cat,
    dog,
    bear,
    rabbit,
    frog,
    lion,
    panda,
    pig,
    mouse,
    penguin,
};
