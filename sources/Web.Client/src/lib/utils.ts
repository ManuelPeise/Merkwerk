/** Same rule as ASP.NET Core Identity on the server (LP-104). */
const minPasswordLength = 10;

export const utils = {
    validation: {
        validateEmail: (email: string) => {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return emailRegex.test(email);
        },
        validatePassword: (password: string) => password.length >= minPasswordLength,
        validateNumber: (value: string) => {
            const numberRegex = /^\d+$/;
            return numberRegex.test(value);
        },
    },
};
