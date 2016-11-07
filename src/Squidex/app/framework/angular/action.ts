/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

const EMPTY_FUNC = () => { };

export const Action = () => {
    return (target: any, key: string) => {
        let observable: any;
        let instance: any;
        let subscriptions: any;
        let subscription: any;

        const subscribe = () => {
            const store = instance.store;

            if (store && observable && observable.subscribe && typeof observable.subscribe === 'function') {
                subscription = observable.subscribe((a: any) => { if (a) { store.next(a); } });

                subscriptions.push(subscription);
            }
        };

        const unsubscribe = () => {
            if (subscription) {
                subscription.unsubscribe();

                subscriptions.splice(subscriptions.indexOf(subscribe), 1);
            }
        };

        if (delete target[key]) {
            Object.defineProperty(target, key, {
                get: () => {
                    return observable;
                },
                set: (v) => {
                    instance = this;

                    if (!instance.___subscriptions) {
                        instance.___subscriptions = [];

                        const destroy = instance.ngOnDestroy ? instance.ngOnDestroy.bind(instance) : EMPTY_FUNC;

                        instance.ngOnDestroy = () => {
                            for (let s of subscriptions) {
                                s.unsubscribe();
                            }

                            instance.___subscriptions = null;

                            destroy();
                        };
                    }

                    subscriptions = instance.___subscriptions;

                    observable = v;

                    unsubscribe();
                    subscribe();
                }
            });
        }
    };
};