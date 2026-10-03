import { utils } from 'src/lib/utils';
import type { NotificationKey } from 'src/lib/translations/translationKeys';

/**
 * Field checks for the auth forms. An empty field has no error yet (the form just stays invalid),
 * so people aren't told off before they typed anything.
 */
export const getEmailError = (email: string): NotificationKey | undefined =>
    email !== '' && !utils.validation.validateEmail(email.trim())
        ? 'notificationEmailInvalid'
        : undefined;

export const getPasswordError = (password: string): NotificationKey | undefined =>
    password !== '' && !utils.validation.validatePassword(password)
        ? 'notificationPasswordTooShort'
        : undefined;

export const getPasswordRepeatError = (
    password: string,
    repeat: string,
): NotificationKey | undefined =>
    repeat !== '' && repeat !== password ? 'notificationPasswordsDoNotMatch' : undefined;
