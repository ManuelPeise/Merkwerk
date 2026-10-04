export interface IUiTestIdProps {
    uiTestId: string;
}

export const uiTestId = (value: string): string => value;

export const uiTestIdOf = (base: string, part: string): string => `${base}-${part}`;
