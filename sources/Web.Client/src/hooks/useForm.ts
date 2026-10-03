import React from 'react';
import { useReducer, type StateUpdate } from 'src/hooks/useReducer';

export type ValidationCallback<TModel> = (state: TModel) => boolean;

export type UseFormResult<TModel> = {
    state: TModel;
    /** True when the state differs from the last model passed to setFormModel (shallow comparison). */
    isModified: boolean;
    isValid: boolean;
    /** Loads a model into the form and makes it the new baseline for isModified. */
    setFormModel: (model: TModel) => void;
    /** Merges a partial update, e.g. updateFormField({ name: event.target.value }). */
    updateFormField: (update: StateUpdate<TModel>) => void;
};

/**
 * Form state with change tracking and validation.
 * Pass a stable validationCallback (module-level function or useCallback), otherwise it runs on every render.
 */
export const useForm = <TModel extends object>(
    initialModel: TModel,
    validationCallback?: ValidationCallback<TModel>,
): UseFormResult<TModel> => {
    const { originalState, state, dispatch, setModel } = useReducer(initialModel);

    const isModified = React.useMemo(() => {
        const keys = [
            ...new Set([...Object.keys(originalState), ...Object.keys(state)]),
        ] as (keyof TModel)[];

        return keys.some((key) => originalState[key] !== state[key]);
    }, [originalState, state]);

    const isValid = React.useMemo(
        () => validationCallback?.(state) ?? true,
        [state, validationCallback],
    );

    return { state, isModified, isValid, setFormModel: setModel, updateFormField: dispatch };
};
