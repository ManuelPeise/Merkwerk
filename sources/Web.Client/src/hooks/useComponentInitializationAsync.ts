import { useEffect, useState } from 'react';

type InitializationCallback = () => Promise<void>;

type InitializationProps<TModel> = {
    isInitialized: boolean;
    model: TModel;
};

const useComponentInitializationAsync = <TModel>(
    callback: InitializationCallback,
): InitializationProps<TModel> => {
    const [initializationProps, setInitializationProps] = useState<InitializationProps<TModel>>({
        isInitialized: false,
        model: {} as TModel,
    });

    useEffect(() => {
        const initialize = async () => {
            await callback();
            setInitializationProps((prev) => ({ ...prev, isInitialized: true }));
        };
        initialize();
    }, [callback]);

    return {
        isInitialized:
            initializationProps.isInitialized && initializationProps.model !== ({} as TModel),
        model: initializationProps.model,
    };
};

export default useComponentInitializationAsync;
