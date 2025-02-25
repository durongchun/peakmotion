document.addEventListener('DOMContentLoaded', function () {
  const searchToggle = document.getElementById('searchToggle');
  const searchForm = document.getElementById('searchForm');
  const navbar = document.querySelector('.navbar');
  const searchClose = document.getElementById('searchClose');

  // Open search
  searchToggle.addEventListener('click', () => {
    // First fade out the search icon
    searchToggle.classList.add('hide');

    // Then expand the navbar
    navbar.classList.add('expanded');

    // Finally show the search form
    setTimeout(() => {
      searchForm.classList.add('expanded');
      searchForm.querySelector('input');
    }, 200);
  });

  // Close search function
  const closeSearch = () => {
    // First hide the search form
    searchForm.classList.remove('expanded');

    // Then collapse the navbar after the form animation is done
    setTimeout(() => {
      navbar.classList.remove('expanded');
    }, 200);

    // Finally show the icon again
    setTimeout(() => {
      searchToggle.classList.remove('hide');
    }, 400);
  };

  // Close on click outside
  document.addEventListener('click', (e) => {
    if (
      !searchForm.contains(e.target) &&
      !searchToggle.contains(e.target) &&
      searchForm.classList.contains('expanded')
    ) {
      closeSearch();
    }
  });

  // Close on X button click
  if (searchClose) {
    searchClose.addEventListener('click', closeSearch);
  }

  // Close on form submit
  searchForm.addEventListener('submit', () => {
    closeSearch();
  });
});
