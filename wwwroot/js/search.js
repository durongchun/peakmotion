document.addEventListener('DOMContentLoaded', function () {
  //   const searchToggle = document.getElementById('searchToggle');
  //   const searchForm = document.getElementById('searchForm');

  const searchToggle = document.getElementById('searchToggle');
  console.log('Search toggle found:', !!searchToggle);
  const searchForm = document.getElementById('searchForm');
  console.log('Search form found:', !!searchForm);

  searchToggle.addEventListener('click', function () {
    searchForm.classList.toggle('expanded');
    if (searchForm.classList.contains('expanded')) {
      searchForm.querySelector('input').focus();
    }
  });
  searchForm.querySelector('input').addEventListener('keypress', function (e) {
    if (e.key === 'Enter') {
      e.preventDefault();
      searchForm.submit();
    }
  });
  // Close search when clicking outside
  document.addEventListener('click', function (event) {
    if (
      !searchForm.contains(event.target) &&
      !searchToggle.contains(event.target) &&
      searchForm.classList.contains('expanded')
    ) {
      searchForm.classList.remove('expanded');
    }
  });
});
