/** Props every form field shares, so all fields behave and look the same. */
export interface IFormFieldProps<TValue> {
    label: string;
    value: TValue;
    /** Form field name; also helps browsers and password managers. */
    name?: string;
    disabled?: boolean;
    required?: boolean;
    /** Validation message (already translated). Puts the field into the error state. */
    errorText?: string;
    /** Hint below the field, shown when there is no error. */
    helperText?: string;
    onChange: (value: TValue) => void;
}
