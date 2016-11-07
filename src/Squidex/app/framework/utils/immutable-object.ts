/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export abstract class ImmutableObject {
    public abstract clone(): ImmutableObject;

    protected afterClone() { }

    protected cloned<T extends ImmutableObject>(updater: (instance: ImmutableObject) => void) {
        const cloned = <T>this.clone();

        updater(cloned);

        cloned.afterClone();

        return cloned;
    }
}