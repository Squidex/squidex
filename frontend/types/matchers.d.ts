/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

declare namespace jasmine {
    interface Matchers<T> {
        toEqualIgnoringProps(expected: Expected<T>, propsToIgnore?: string[]): boolean;
    }
}