
  const themeMap = {
    dark: "light",
    light: "dark"
  };
  
  function load() {
  const theme = localStorage.getItem('theme')
    || (tmp = Object.keys(themeMap)[0],
        localStorage.setItem('theme', tmp),
        tmp);
  const bodyClass = document.body.classList;
  bodyClass.add(theme);
  }
  
  function toggleTheme() {
    const current = localStorage.getItem('theme');
    const next = themeMap[current];
  const bodyClass = document.body.classList;
    bodyClass.replace(current, next);
    localStorage.setItem('theme', next);
  }