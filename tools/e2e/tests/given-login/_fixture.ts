import { test as base } from '@playwright/test';

type LoginFixture = {
    userEmail: string;
    userPassword: string;
};

export const test = base.extend<LoginFixture>({
    userEmail: [
        'hello@squidex.io',
        { option: true },
    ],
    userPassword: [
        '1q2w3e$R',
        { option: true },
    ],
});

export { expect } from '@playwright/test';

