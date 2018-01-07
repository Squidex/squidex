/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export class RouterMockup {
    public lastNavigation: any[];

    public navigate(target: any[]) {
        this.lastNavigation = target;
    }
}