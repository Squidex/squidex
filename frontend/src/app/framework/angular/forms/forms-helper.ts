/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from '../../utils/types';

export interface FocusComponent {
    focus(): void;
}

export function isValidValue(value: any) {
    return !Types.isNull(value) && !Types.isUndefined(value);
}
