export const utils = {
    validation: {
        validateEmail: (email: string) => {
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
            return emailRegex.test(email);
        },
        validateNumber: (value: string) => {
            const numberRegex = /^\d+$/;
            return numberRegex.test(value);
        },
    },
};
