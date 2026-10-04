import React from 'react';

/** Either a partial state or a function that computes it from the current state (like setState). */
export type StateUpdate<TState> = Partial<TState> | ((state: TState) => Partial<TState>);

type ReducerAction<TState> =
    { type: 'merge'; update: StateUpdate<TState> } | { type: 'replace'; state: TState };

const isUpdater = <TState>(
    update: StateUpdate<TState>,
): update is (state: TState) => Partial<TState> => typeof update === 'function';

/**
 * Merges a partial update into the state (shallow). Returns the same state object when nothing changed,
 * so React skips the re-render.
 */
export const mergeState = <TState extends object>(
    state: TState,
    update: StateUpdate<TState>,
): TState => {
    const changes = isUpdater(update) ? update(state) : update;

    if (!changes) {
        return state;
    }

    const changedKeys = Object.keys(changes) as (keyof TState)[];
    const isStateChanged = changedKeys.some((key) => state[key] !== changes[key]);

    return isStateChanged ? { ...state, ...changes } : state;
};

const reducerFunction = <TState extends object>(
    state: TState,
    action: ReducerAction<TState>,
): TState => (action.type === 'replace' ? action.state : mergeState(state, action.update));

export type UseReducerResult<TModel> = {
    /** Model from the last setModel call (or the initial model) - the baseline for change tracking. */
    originalState: TModel;
    state: TModel;
    /** Merges a partial update into the state. Stable reference. */
    dispatch: (update: StateUpdate<TModel>) => void;
    /** Replaces state and baseline, e.g. after loading a record. Stable reference. */
    setModel: (model: TModel) => void;
};

/**
 * State object with merge semantics, so related values change together in one render.
 *
 * @example
 * const { state, dispatch, setModel } = useReducer({ isLoading: false, items: [] as ExerciseDto[] });
 * dispatch({ isLoading: true });
 * dispatch((current) => ({ items: [...current.items, item] }));
 */
export const useReducer = <TModel extends object>(
    initialModel: TModel,
): UseReducerResult<TModel> => {
    const [originalState, setOriginalState] = React.useState(initialModel);
    const [state, dispatchAction] = React.useReducer(reducerFunction<TModel>, initialModel);

    const dispatch = React.useCallback(
        (update: StateUpdate<TModel>) => dispatchAction({ type: 'merge', update }),
        [],
    );

    const setModel = React.useCallback((model: TModel) => {
        setOriginalState(model);
        dispatchAction({ type: 'replace', state: model });
    }, []);

    return { originalState, state, dispatch, setModel };
};
