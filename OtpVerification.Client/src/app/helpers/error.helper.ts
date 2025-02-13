export class ErrorHelper {
    public static formatHttpError({ error }: any): string {
        if (!error) {
            return "";
        }

        if (typeof error === 'string') {
            return error;
        }

        if (typeof error?.errors === 'object') {
            return Object.values(error.errors).join('');
        }

        return "";
    }
}