/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

export class RouterMockup {
    public lastNavigation: any[];

    public navigate(target: any[]) {
        this.lastNavigation = target;
    }
}