/*
 *PinkParrot CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

const EMPTY_FUNC = () => { };

export function Action() {
    return function (target: any, key: string) {
        let observable: any;
        let instance: any;
        let subscriptions: any;
        let subscription: any;

        function subscribe() {
            const store = instance.store;

            if (store && observable && observable.subscribe && typeof observable.subscribe === 'function') {
                subscription = observable.subscribe((a: any) => { if (a) { store.next(a); } });

                subscriptions.push(subscription);
            }
        };

        function unsubscribe() {
            if (subscription) {
                subscription.unsubscribe();

                subscriptions.splice(subscriptions.indexOf(subscribe), 1);
            }
        };

        if (delete target[key]) {
            Object.defineProperty(target, key, {
                get: function () {
                    return observable;
                },
                set: function (v) {
                    instance = this;

                    if (!instance.___subscriptions) {
                        instance.___subscriptions = [];

                        let destroy = instance.ngOnDestroy ? instance.ngOnDestroy.bind(instance) : EMPTY_FUNC;

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
}