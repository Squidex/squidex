import { browser } from 'protractor';

import {
  BrowserUtil,
  constants,
  Users
} from './../../utils';

import {
  HomePage,
  LoginPage
} from './../../pages';

describe('User Login', () => {
  const authors = Users;
  let loginPage: LoginPage;
  let homePage: HomePage;
  let browserPage: BrowserUtil;

  beforeAll(async () => {
    loginPage = new LoginPage();
    homePage = new HomePage();
    browserPage = new BrowserUtil();
    await loginPage.login(
      authors.find(obj => {
        return obj.name === 'vegaEditor';
      })
    );
  });

  afterAll(() => {
    homePage.userLogout();
  });


  it('Login with Vega Editor credentials', async () => {
    expect(await browserPage.getCurrentURL()).toBe(
      `${browser.params.baseUrl}/app`
    );
    const text = await homePage.userNameDisplay();
    expect(text).toEqual(constants.loginTest.editorWelcomeMessage);
  });
});
